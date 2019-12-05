import tensorflow as tf

def weight_variable(shape, name=None):
    # initialize weighted variables.
    initial = tf.truncated_normal(shape, stddev=0.1, seed=1)
    return tf.Variable(initial, dtype=tf.float32, name=name)

def conv2d(x, W, strides=[1, 1, 1, 1], p='SAME', name=None):
    # set convolution layers.
    assert isinstance(x, tf.Tensor)
    return tf.nn.conv2d(x, W, strides=strides, padding=p, name=name)

def batch_norm(net, train=True):
    batch, rows, cols, channels = [i.value for i in net.get_shape()]
    var_shape = [channels]
    mu, sigma_sq = tf.nn.moments(net, [1,2], keep_dims=True)
    shift = tf.Variable(tf.zeros(var_shape))
    scale = tf.Variable(tf.ones(var_shape))
    epsilon = 1e-3
    normalized = (net-mu)/(sigma_sq + epsilon)**(.5)
    return scale * normalized + shift

def relu(x):
    assert isinstance(x, tf.Tensor)
    return tf.nn.relu(x)

def deconv2d(x, W, strides=[1, 1, 1, 1], p='SAME', name=None):
    assert isinstance(x, tf.Tensor)
    _, _, c, _ = W.get_shape().as_list()
    b, h, w, _ = x.get_shape().as_list()
    return tf.nn.conv2d_transpose(x, W, [b, strides[1] * h, strides[1] * w, c], strides=strides, padding=p, name=name)

def max_pool_2x2(x):
    assert isinstance(x, tf.Tensor)
    return tf.nn.max_pool(x, ksize=[1, 2, 2, 1], strides=[1, 2, 2, 1], padding='SAME')


class ResidualBlock():
    def __init__(self, idx, ksize=3, train=False, data_dict=None):
        self.W1 = weight_variable([ksize, ksize, 128, 128], name='R'+ str(idx) + '_conv1_w')
        self.W2 = weight_variable([ksize, ksize, 128, 128], name='R'+ str(idx) + '_conv2_w')
    def __call__(self, x, idx, strides=[1, 1, 1, 1]):
        h = relu(batch_norm(conv2d(x, self.W1, strides, name='R' + str(idx) + '_conv1')))
        h = batch_norm(conv2d(h, self.W2, name='R' + str(idx) + '_conv2'))
        return x + h



def net(image):
    c1 = weight_variable([9, 9, 3, 32], name='t_conv1_w')
    c2 = weight_variable([4, 4, 32, 64], name='t_conv2_w')
    c3 = weight_variable([4, 4, 64, 128], name='t_conv3_w')
    r1 = ResidualBlock(1, train=True)
    r2 = ResidualBlock(2, train=True)
    r3 = ResidualBlock(3, train=True)
    r4 = ResidualBlock(4, train=True)
    r5 = ResidualBlock(5, train=True)
    d1 = weight_variable([4, 4, 64, 128], name='t_dconv1_w')
    d2 = weight_variable([4, 4, 32, 64], name='t_dconv2_w')
    d3 = weight_variable([9, 9, 3, 32], name='t_dconv3_w')

    h = relu(batch_norm(conv2d(image, c1, name='t_conv1')))
    h = relu(batch_norm(conv2d(h, c2, strides=[1, 2, 2, 1], name='t_conv2')))
    h = relu(batch_norm(conv2d(h, c3, strides=[1, 2, 2, 1], name='t_conv3')))

    h = r1(h, 1)
    h = r2(h, 2)
    h = r3(h, 3)
    h = r4(h, 4)
    h = r5(h, 5)

    h = relu(batch_norm(deconv2d(h, d1, strides=[1, 2, 2, 1], name='t_deconv1')))
    h = relu(batch_norm(deconv2d(h, d2, strides=[1, 2, 2, 1], name='t_deconv2')))
    #y = batch_norm(conv2d(h, d3, name='t_deconv3'))
    y = batch_norm(deconv2d(h, d3, name='t_deconv3'))
    return tf.nn.tanh(y) * 150 + 127.5
