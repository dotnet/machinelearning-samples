from __future__ import print_function

import argparse
import numpy as np
import os
import shutil
import sys
import time

sys.path.insert(0, '.')

def parse_arguments():
    parser = argparse.ArgumentParser(description='Real-time style transfer')
    parser.add_argument('--input_dir', type=str,
                        help='Top-level data directory path (according to the paper, use MSCOCO 80k images)')
    parser.add_argument('--output_dir', type=str, help='Checkpoint & SavedModel directory.')
    parser.add_argument('--log_dir', type=str, help='TensorBoard directory.')
    parser.add_argument('--gpu_id', type=int, default=-1, help='GPU ID (negative value indicates CPU)')
    parser.add_argument('--set_cuda_visible_devices', type=bool, default=True, help='Set CUDA_VISIBLE_DEVICES to gpu_id')
    parser.add_argument('--style_image', type=str, default='starry_night.jpg', help='Style image')
    parser.add_argument('--batch_size', type=int, default=1, help='Batch size for training (default value is 16)')
    parser.add_argument('--epoch', default=2, type=int, help='Epoch count.')
    parser.add_argument('--lambda_tv', type=float, default=10e-4,
                        help='Weight of total variation regularization according to the paper to be set between 10e-4 and 10e-6.')
    parser.add_argument('--lambda_feat', type=float, default=7.5e0)
    parser.add_argument('--lambda_style', type=float, default=15)
    parser.add_argument('--lr', default=1e-3, type=float, help='Initial learning rate.')
    options, unknown = parser.parse_known_args()
    if len(unknown):
        print('Unknown arguments: ' + ','.join(unknown))
    return options


options =  parse_arguments()

# Set the computation device
device = '/cpu:0'
if options.gpu_id >= 0:
    if options.set_cuda_visible_devices:
        os.environ['CUDA_VISIBLE_DEVICES'] = str(options.gpu_id)
        device = '/gpu:0'
    else:
        device = '/gpu:%d' % options.gpu_id


import tensorflow as tf
import vgg
import stylenet
from utils import info, error, fail, get_available_gpus, list_jpgs, read_img


def main():
    global options, device

    # Get the ENV context
    script_dir = os.path.dirname(__file__)
    env = os.environ.copy()
    
    # Set the input folder
    input_dir = os.path.expanduser(options.input_dir) if options.input_dir \
        else os.path.join(script_dir, '..', 'data')
    vgg_path = os.path.join(input_dir, 'vgg', 'imagenet-vgg-verydeep-19.mat')
    coco_dir = os.path.join(input_dir, 'train')
    if not os.path.isdir(input_dir):
        fail('Failed to find the input folder at ' + input_dir)
    if not os.path.isfile(vgg_path):
        error('Failed to find the VGG model file at ' + vgg_path)
        fail('Please download it from http://www.vlfeat.org/matconvnet/models/beta16/imagenet-vgg-verydeep-19.mat')
    if not os.path.isdir(coco_dir):
        error('Failed to find the COCO 2014 training images in ' + coco_dir)
        fail('Plese download it from http://images.cocodataset.org/zips/train2014.zip')

    # Set the output folder
    output_dir = os.path.expanduser(options.output_dir) if options.output_dir \
        else env.get('OUTPUT_DIR', os.path.join(script_dir, '..', 'output'))
    model_dir = os.path.join(output_dir, 'checkpoint')
    if os.path.isdir(output_dir):
        if not os.path.isdir(model_dir):
            info('Creating a folder to store checkpoint at ' + model_dir)
            os.makedirs(model_dir)
    else:
        info('Creating a folder to store checkpoint at ' + model_dir)
        os.makedirs(model_dir)
    
    # Set the TensorBoard folder
    log_dir = os.path.expanduser(options.log_dir) if options.log_dir \
        else env.get('LOG_DIR', os.path.join(script_dir, '..', 'log'))
    if not os.path.isdir(log_dir):
        info('Creating a folder to store TensorBoard events at ' + log_dir)
        os.makedirs(log_dir)
    
    # Set the style image path
    style_path = os.path.expanduser(options.style_image) if os.path.isfile(options.style_image) \
        else os.path.join(input_dir, 'style_images', options.style_image)
    style_name = os.path.basename(os.path.splitext(style_path)[0])
    ckpt_path = os.path.join(model_dir, style_name + '.ckpt')
    if not os.path.isfile(style_path):
        fail('Failed to find the style image at ' + style_path)

    # Set hyper parameters
    batch_size = options.batch_size
    epochs = options.epoch
    lr = options.lr
    lambda_tv = options.lambda_tv
    lambda_feat = options.lambda_feat
    lambda_style = options.lambda_style
    
    # Print parsed arguments
    info('--------- Training parameters -------->')
    info('Style image path: ' + style_path)
    info('VGG model path: ' + vgg_path)
    info('Training image dir: ' + coco_dir)
    info('Checkpoint path: ' + ckpt_path)
    info('TensorBoard log dir: ' + log_dir)
    info('Training device: ' + device)
    info('Batch size: %d' % batch_size)
    info('Epoch count: %d' % epochs)
    info('Learning rate: ' + str(lr))
    info('Lambda tv: ' + str(lambda_tv))
    info('Lambda feat: ' + str(lambda_feat))
    info('Lambda style: ' + str(lambda_style))
    info('<-------- Training parameters ---------')

    # COCO images to train
    content_targets = list_jpgs(coco_dir)
    if len(content_targets) % batch_size != 0:
        content_targets = content_targets[:-(len(content_targets) % batch_size)]
    info('Total training data size: %d' % len(content_targets))
    
    # Image shape
    image_shape = (224, 224, 3)
    batch_shape = (batch_size,) + image_shape
    
    # Style target
    style_target = read_img(style_path)
    style_shape = (1,) + style_target.shape
    
    with tf.device(device), tf.Session() as sess:
        # Compute gram maxtrix of style target
        style_image = tf.placeholder(tf.float32, shape=style_shape, name='style_image')
        vggstyletarget = vgg.net(vgg_path, vgg.preprocess(style_image))
        style_vgg = vgg.get_style_vgg(vggstyletarget, style_image, np.array([style_target]))
        
        # Content target feature 
        content_vgg = {}
        inputs = tf.placeholder(tf.float32, shape=batch_shape, name='inputs')
        content_net = vgg.net(vgg_path, vgg.preprocess(inputs))
        content_vgg['relu4_2'] = content_net['relu4_2']
        
        # Feature after transformation
        outputs = stylenet.net(inputs / 255.0)
        vggoutputs = vgg.net(vgg_path, vgg.preprocess(outputs))
        
        # Compute feature loss
        loss_f = options.lambda_feat * vgg.total_content_loss(vggoutputs, content_vgg, batch_size)
        
        # Compute style loss        
        loss_s = options.lambda_style * vgg.total_style_loss(vggoutputs, style_vgg, batch_size)
        
        # Total variation denoising
        loss_tv = options.lambda_tv * vgg.total_variation_regularization(outputs, batch_size, batch_shape)
        
        # Total loss
        total_loss = loss_f + loss_s + loss_tv
        train_step = tf.train.AdamOptimizer(options.lr).minimize(total_loss)
        
        # Create summary
        tf.summary.scalar('loss', total_loss)
        merged = tf.summary.merge_all()
        
        # Used to save model
        saver = tf.train.Saver()
    
    with tf.Session(config=tf.ConfigProto(allow_soft_placement=True)) as sess:
        # Restore checkpoint if available
        sess.run(tf.global_variables_initializer())
        ckpt = tf.train.get_checkpoint_state(model_dir)
        if ckpt and ckpt.model_checkpoint_path:
            info('Restoring from ' + ckpt.model_checkpoint_path)
            saver.restore(sess, ckpt.model_checkpoint_path)
        
        # Write the graph
        writer = tf.summary.FileWriter(log_dir, sess.graph)
        
        # Start to train
        total_step = 0
        for epoch in range(epochs):
            info('epoch: %d' % epoch)
            step = 0
            while step * batch_size < len(content_targets):
                time_start = time.time()
                
                # Load one batch
                batch = np.zeros(batch_shape, dtype=np.float32)
                for i, img in enumerate(content_targets[step * batch_size : (step + 1) * batch_size]):
                    batch[i] = read_img(img, image_shape).astype(np.float32) # (224,224,3)
                
                # Proceed one step
                step += 1
                total_step += 1
                _, loss, summary = sess.run([train_step, total_loss, merged], feed_dict= {inputs: batch})
             
                time_elapse = time.time() - time_start
                if total_step % 5 == 0:
                    info('[step {}] elapse time: {} loss: {}'.format(total_step, time_elapse, loss))
                    writer.add_summary(summary, total_step)
                
                # Write checkpoint
                if total_step % 2000 == 0:                                        
                    info('Saving checkpoint to ' + ckpt_path)
                    saver.save(sess, ckpt_path, global_step=total_step)
        
        info('Saving final checkpoint to ' + ckpt_path)
        saver.save(sess, ckpt_path, global_step=total_step)


if __name__ == '__main__':
    main()
