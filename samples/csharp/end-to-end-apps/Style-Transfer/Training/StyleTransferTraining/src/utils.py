import numpy as np
import os
import scipy.misc
import sys

from tensorflow.python.client import device_lib


def info(message):
    print(message)


def error(message):
    sys.stderr.write(message)
    sys.stderr.write(os.linesep)


def fail(message):
    sys.stderr.write(message)
    sys.stderr.write(os.linesep)
    sys.exit(1)


def get_available_gpus():
    local_device_protos = device_lib.list_local_devices()
    return [x.name for x in local_device_protos if x.device_type == 'GPU']


def list_jpgs(root_dir):
    jpgs = []
    for root, _, files in os.walk(root_dir):
        for file in files:
            if file.endswith('jpg'):
                 jpgs.append(os.path.join(root, file))
    return jpgs


def read_img(src, new_size = None):
   img = scipy.misc.imread(src, mode='RGB')
   if new_size:
       img = scipy.misc.imresize(img, new_size)
   return img


def save_img(out_path, img):
    img = np.clip(img, 0, 255).astype(np.uint8)
    scipy.misc.imsave(out_path, img)
