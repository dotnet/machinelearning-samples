
# Style Transfer Experiment
Demo for style transfer

## Prerequisites
- Training data images using [Microsoft COCO](http://cocodataset.org]) 
- [VGG-19 Mat](http://www.vlfeat.org/matconvnet/pretrained/)

## Reference
- The project is based on the paper: [Perceptual Losses for Real-Time Style Transfer and Super-Resolution](https://arxiv.org/abs/1603.08155)
- The training code in this repository mostly based on the following nice work, thanks to the author.[tensorflow-fast-neuralstyle](https://github.com/antlerros/tensorflow-fast-neuralstyle)
- We also leveraged work from the following projects, thanks to the author
    - [neural-style](https://github.com/anishathalye/neural-style)
    - [fast-style-transfer](https://github.com/lengstrom/fast-style-transfer)

## Step notes

- Copy training images into data/style_images using gsutil (install it via `curl https://sdk.cloud.google.com | bash`, run `gsutil -m rsync gs://images.cocodataset.org/train2014 .` in the directory (this downloads 12.6GB of data)
- Copy the model `curl http://www.vlfeat.org/matconvnet/models/beta16/imagenet-vgg-verydeep-19.mat > imagenet-vgg-verydeep-19.mat` in the data/vgg directory
- `mkdir /output`
- `mkdir /log`
- Update train.py to set batch size to 16 (or pass as param)