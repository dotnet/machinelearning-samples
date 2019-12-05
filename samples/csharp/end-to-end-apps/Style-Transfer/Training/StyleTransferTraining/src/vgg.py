# Copyright (c) 2015-2016 Anish Athalye. Released under GPLv3.

import tensorflow as tf
import numpy as np
import scipy.io
import functools


MEAN_PIXEL = np.array([ 123.68 ,  116.779,  103.939])
FEATURES = ('relu1_1', 'relu2_1', 'relu3_1', 'relu4_1', 'relu5_1')


def net(data_path, input_image):
    layers = (
        'conv1_1', 'relu1_1', 'conv1_2', 'relu1_2', 'pool1',
        'conv2_1', 'relu2_1', 'conv2_2', 'relu2_2', 'pool2',
        'conv3_1', 'relu3_1', 'conv3_2', 'relu3_2', 'conv3_3',
        'relu3_3', 'conv3_4', 'relu3_4', 'pool3',
        'conv4_1', 'relu4_1', 'conv4_2', 'relu4_2', 'conv4_3',
        'relu4_3', 'conv4_4', 'relu4_4', 'pool4',
        'conv5_1', 'relu5_1', 'conv5_2', 'relu5_2', 'conv5_3',
        'relu5_3', 'conv5_4', 'relu5_4'
    )
    data = scipy.io.loadmat(data_path)
    weights = data['layers'][0]
    
    net = {}
    current = input_image
    for i, name in enumerate(layers):
        kind = name[:4]
        if kind == 'conv':
            kernels, bias = weights[i][0][0][0][0]
            # matconvnet: weights are [width, height, in_channels, out_channels]
            # tensorflow: weights are [height, width, in_channels, out_channels]
            kernels = np.transpose(kernels, (1, 0, 2, 3))
            bias = bias.reshape(-1)
            current = _conv_layer(current, kernels, bias)
        elif kind == 'relu':
            current = tf.nn.relu(current)
        elif kind == 'pool':
            current = _pool_layer(current)
        net[name] = current
    
    assert len(net) == len(layers)
    return net


def _conv_layer(input, weights, bias):
    conv = tf.nn.conv2d(input, tf.constant(weights), strides=(1, 1, 1, 1),
            padding='SAME')
    return tf.nn.bias_add(conv, bias)


def _pool_layer(input):
    return tf.nn.max_pool(input, ksize=(1, 2, 2, 1), strides=(1, 2, 2, 1),
            padding='SAME')


def preprocess(image):
    return image - MEAN_PIXEL


def unprocess(image):
    return image + MEAN_PIXEL


def get_style_vgg(vggstyletarget, style_image, style_pre):
    style_vgg = {}
    for layer in FEATURES:
        features = vggstyletarget[layer].eval(feed_dict={style_image:style_pre})
        features = np.reshape(features, (-1, features.shape[3]))
        gram = np.matmul(features.T, features) / features.size
        style_vgg[layer] = gram
    return style_vgg


# total variation denoising
def total_variation_regularization(x, bsize, bshape, beta=1):
    assert isinstance(x, tf.Tensor)
    tv_y_size = _tensor_size(x[:,1:,:,:])
    tv_x_size = _tensor_size(x[:,:,1:,:])
    y_tv = tf.nn.l2_loss(x[:,1:,:,:] - x[:,:bshape[1]-1,:,:])
    x_tv = tf.nn.l2_loss(x[:,:,1:,:] - x[:,:,:bshape[2]-1,:])
    tv = 2*(x_tv/tv_x_size + y_tv/tv_y_size)/bsize
    return tv


def total_style_loss(net, style_vgg, bsize):
    style_losses = []
    for style_layer in FEATURES:
        layer = net[style_layer]
        bs, height, width, filters = map(lambda i:i.value,layer.get_shape())
        size = height * width * filters
        feats = tf.reshape(layer, (bs, height * width, filters))
        feats_T = tf.transpose(feats, perm=[0,2,1])
        grams = tf.matmul(feats_T, feats) / size
        style_gram = style_vgg[style_layer]
        style_losses.append(2 * tf.nn.l2_loss(grams - style_gram)/style_gram.size)
    return functools.reduce(tf.add, style_losses) / bsize


def total_content_loss(net, content_vgg, bsize):
    content_size = _tensor_size(content_vgg['relu4_2'])*bsize
    assert _tensor_size(content_vgg['relu4_2']) == _tensor_size(net['relu4_2'])
    return 2 * tf.nn.l2_loss(net['relu4_2'] - content_vgg['relu4_2']) / content_size


def _tensor_size(tensor):
    from operator import mul
    return functools.reduce(mul, (d.value for d in tensor.get_shape()[1:]), 1)
