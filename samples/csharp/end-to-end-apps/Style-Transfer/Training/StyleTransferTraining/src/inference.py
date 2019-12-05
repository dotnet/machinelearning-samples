import sys
sys.path.insert(0, '.')
import stylenet, utils
import numpy as np
import argparse
import tensorflow as tf
import os

parser = argparse.ArgumentParser(description='Real-time style transfer image generator')
parser.add_argument('--input', '-i', required=True, type=str, help='content image')
parser.add_argument('--gpu', '-g', default=-1, type=int,
                    help='GPU ID (negative value indicates CPU)')
parser.add_argument('--ckpt', '-c', type=str, help='checkpoint to be loaded')
parser.add_argument('--mdl', '-m', type=str, help='savedmodel to be loaded')
parser.add_argument('--out', '-o', default='stylized_image.jpg', type=str, help='stylized image\'s name')

args = parser.parse_args()

outfile_path = args.out
content_image_path = args.input
gpu = args.gpu

if not args.ckpt and not args.mdl:
    utils.fail('Please provide the folder path of either checkpoint or savedmodel')

if gpu > -1:
    device = '/gpu:{}'.format(gpu)
else:
    device = '/cpu:0'

if args.ckpt:
    # Get the input shape
    original_image = utils.read_img(content_image_path).astype(np.float32) / 255.0
    shaped_input = original_image.reshape((1,) + original_image.shape)

    with tf.device(device):
        # Construct inference graph based on the input shape
        inputs = tf.placeholder(tf.float32, shaped_input.shape, name='input')
        net = stylenet.net(inputs)
        saver = tf.train.Saver(restore_sequentially=True)

        # Transfer image style
        with tf.Session(config=tf.ConfigProto(allow_soft_placement=True)) as sess:                
            input_checkpoint = tf.train.get_checkpoint_state(args.ckpt)
            saver.restore(sess, input_checkpoint.model_checkpoint_path)           
            out = sess.run(net, feed_dict={inputs: shaped_input})
else:
    with tf.Session(config=tf.ConfigProto(allow_soft_placement=True)) as sess:
        # Load the infernece graph and get the input shape
        meta_graph = tf.saved_model.loader.load(sess, [tf.saved_model.tag_constants.SERVING], args.mdl)
        signature = meta_graph.signature_def['Transfer']
        input_name = signature.inputs[tf.saved_model.signature_constants.PREDICT_INPUTS].name
        output_name = signature.outputs[tf.saved_model.signature_constants.PREDICT_OUTPUTS].name
        inputs = tf.get_default_graph().get_tensor_by_name(input_name)

        # Read the image and resize it to the desired size
        original_image = utils.read_img(content_image_path, inputs.shape.as_list()[1:]).astype(np.float32) / 255.0
        shaped_input = original_image.reshape((1,) + original_image.shape)

        # Transfer image style
        net = tf.get_default_graph().get_tensor_by_name(output_name)
        out = sess.run(net, feed_dict={inputs: shaped_input})

utils.save_img(outfile_path, out.reshape(out.shape[1:]))
