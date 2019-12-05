# Style Transfer Lab - Asp.Net core Web/service Sample

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.4           | Dynamic API | up-to-date | Console app | Images | Style transfer | TensorFlow model  | DeepLearning model |

## Problem
The problem is how to build and train a TensorFlow model using a Deep Learning Virtual Machine (DLVM) and create predictions using ML.NET in a web app/service while using in-memory images.

## Solution
This lab is based on the [Style Transfer](https://styletransfers.azurewebsites.net/) project that uses Artificial Intelligence to create art. The AI algorithm applies different transformations to the image to create a new one in the style of a specific painting.

For the purposes of the lab we will create one model that reflects a specific style as creating new models is time consuming. We will use a DLVM with GPUs to train the model and we'll use that model in an application. We'll show you how easy is to get predictions from a TensorFlow model using ML.NET.

## Introduction

Learn about the libraries used in this lab and how to set up your environment.

### A) Concepts overview for this Lab

For the purposes of the lab we will create one model that reflects a specific style as creating new models is time consuming. We will use a DLVM with GPUs to train the model and we'll use that model in an application. We'll show you how easy is to get predictions from a TensorFlow model using ML.NET.

* **TensorFlow:** a popular deep learning and machine learning toolkit that trains deep neural networks (and general numeric computations). ML.NET can use TensorFlow models for scoring, allowing us to easily host powerful neural networks within our .NET applications.

* **ML.NET:** Cross-platform, open source machine learning framework for .NET developers.

* **ML.NET Transformer:** a transformer is a component that takes data, does some work on it, and returns new transformed data. Most transformers in ML.NET tend to operate on one *input* column at a time and produce an *output* column.

* **ML.NET Learning Pipeline:** a component that chains together different estimators. The result of the learning pipeline is a transformer.

* **ML.NET Prediction Function:** a component that applies a transformer to one row. Once you get the model (a transformer either trained via `Fit()`, or loaded from somewhere), you can use it to make predictions using the normal calls to `model.Transform(data)`.

* [**Netron**](https://github.com/lutzroeder/Netron) is a model graph viewer. This allows us to get information about the *input* and *output* columns from the nodes in our model.

### B) Setup your Azure subscription

This lab **requires** an Azure subscription.

If you need a new Azure subscription, then there are a couple of options to get a free subscription:

1. The easiest way to sign up for an Azure subscription is with VS Dev Essentials and a personal Microsoft account (like @outlook.com). This does require a credit card; however, there is a spending limit in the subscription so it won't be charged unless you explicitly remove the limit.
    * Open Microsoft Edge and go to the [Microsoft VS Dev Essentials site](https://visualstudio.microsoft.com/dev-essentials/).
    * Click **Join or access now**.
    * Sign in using your personal Microsoft account.
    * If prompted, click Confirm to agree to the terms and conditions.
    * Find the Azure tile and click the **Activate** link.
1. Alternatively, if the above isn't suitable, you can sign up for a free Azure trial.
    * Open Microsoft Edge and go to the [free Azure trial page](https://azure.microsoft.com/en-us/free/).
    * Click **Start free**.
    * Sign in using your personal Microsoft account.
1. Complete the Azure sign up steps and wait for the subscription to be provisioned. This usually only takes a couple of minutes.

### C) Install Visual Studio Code

We'll use VS Code to test our model using ML.NET. Install VS Code along the following packages in your local environment to run the sample project.

1. Download Visual Studio Code from [https://code.visualstudio.com/download](https://code.visualstudio.com/download).
1. Install the [C# Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp).

You will also require the following libraries in order to run the code:

- [.NET Core SDK 2.2](https://dotnet.microsoft.com/download)
- [Node.js >= 8](https://nodejs.org/en/download/)

### D) Create a Deep Learning Virtual Machine

The Deep Learning Virtual Machine is a specially configured variant of the [Data Science Virtual Machine (DSVM)](https://docs.microsoft.com/en-us/azure/machine-learning/data-science-virtual-machine/overview) designed to use GPUs to more efficiently train deep neural networks.

1. Log into the [Azure Portal](https://portal.azure.com)(portal.azure.com).
1. Click **Create Resource [+]**  from the left menu and search for `Deep Learning Virtual Machine`.
1. Select the first result and click the **Create** button.
1. Provide the required information:
    * Name: `ai-labs-st-<your initials>`.
    * OS Type: `Linux`.
    * Set the username and password.
      > NOTE: keep these credentials in a Notepad as we'll need them later to connect to the VM.

    * Select your preferred subscription.
    * Create a new resource group: `mlnet-<your initials>`.
    * Location: `West US 2`
        > Note: for this lab we'll use a Deep Learning VM which requires NC class machines that are only available in EAST US, NORTH CENTRAL US, SOUTH CENTRAL US, and WEST US 2.
        
1. Click **OK** to continue with the **Settings** section.
1. Make sure `1x Standard NC6` is selected for the VM size.
1. Continue until the last section **Buy**.
1. Click **Create** to start the provisioning.
   > NOTE: a link is provided to the terms of the transaction. The VM does not have any additional charges beyond the compute for the server size you chose in the size step.

1. The provisioning should take about 10 minutes. The status of the provisioning is displayed int the Azure portal.
1. Once provisioning is complete, you will see a **Deployment succeeded** notification.
1. Go to **All Resources** in the left pane and search for the new resource: `ai-labs-st-<your initials>`.
1. Click on the first result to open it.
1. Copy the `Public IP address` into Notepad.
   > NOTE: we'll need this value later on to connect to the VM.


## Train a model using a Deep Learning VM (DLVM)

Once you have provisioned your Deep Learning Virtual Machine (DLVM), you can start building deep neural network models. For this lab we will use TensorFlow to create a model that will perform style transfers on images.

### A) Download the lab materials

Follow the following steps to download the sample code provided for this lab. It includes the scripts to train and test your model.

1. Login to your VM:
    * Open a command prompt and run `ssh <username>@<dlvm public ip address>`
      > NOTE: the **username** is the one you indicated when you created the DLVM.

    * After that, you'll be prompted for your password. Type the one you used during the DLVM setup.

1. You should see a welcome message in your terminal indicating that you have successfully connected to the DLVM.
1. Clone this repo to your VM using the command `git clone https://github.com/microsoft/AISchoolTutorials ai-school-tutorials`.
1. Copy the following command to move the lab content to `<your home>\styletransfer-lab`: `mv ai-school-tutorials/style-transfer ./styletransfer-lab`
    > ALERT: make sure to put your code into `<your home>\styletransfer-lab`.

### B) Download the images dataset

After connecting to the DLVM, you'll need to download the images dataset for training.

1. Enter the following command to navigate to the training directory: `cd styletransfer-lab/Training/StyleTransferTraining`.
    > NOTE: there you'll see the **data**, **output** and **src** folders. 

1. Enter the following commands to download the images from Google Cloud Storage:
    * Install **gsutil**: `curl https://sdk.cloud.google.com | bash`
    * Make sure to add **gsutil** to the system *PATH* when prompted. Use the default *bashrc* file.
    * Type the following command to apply the changes in the *.bashrc* file: `source ~/.bashrc`
    * Download the images: `gsutil -m rsync gs://images.cocodataset.org/train2014 data/train`
        > NOTE: this process might take a few minutes as it will download ˜12.6GB of data.

    * Check that you have the training images: `du -sh data/train`
        > NOTE: this should output ˜13GB of data.

1. Once the images have been downloaded, execute the following command to download the base model: `curl http://www.vlfeat.org/matconvnet/models/beta16/imagenet-vgg-verydeep-19.mat > data/vgg/imagenet-vgg-verydeep-19.mat`
    > NOTE: this is a pretrained model from the Very Deep Convolutional Networks for Large-Scale Visual Recognition.

### C) Train and create the model

Create the TensorFlow model using the previously downloaded images.

1. Navigate to the **src** folder: `cd src`.
1. Run the training script: `python train.py --input_dir ../data --output_dir ../output --log_dir ../log --gpu_id 0 --batch_size 16`
   > ALERT: the training lasts for about 4 hours, so consider using a tool like [screen](https://linuxize.com/post/how-to-use-linux-screen/) so you can keep your process running if the ssh connection fails.

   > NOTE: the parameters indicate the training images path, the output and log directories, the GPU to use, and batch size that will be used in the training process.

4. Once the training is finished, check the **output** directory: `ls ../output/checkpoint`. You should see the following files:

![Checkpoints](Resources/checkpoints.png)

### D) Export the model

Export the model checkpoint to a saved model.

1. Run the following command: `python export.py --ckpt_dir ../output/checkpoint`.
   > NOTE: make sure you are in the **src** folder. This might take a few minutes.

1. When the process is finished it will create an **export** directory. Type `ls export` and you should see a **saved_model.pb** file and a **variables** folder.

1. **Optional:** You can run the **inference.py** script to test the model. In order to execute the script you need to run  `python inference.py --input ../data/style_images/starry_night.jpg --gpu 0 --mdl ./export/` from the **src** directory.  This will take the input image (*starry_night.jpg*) and run it through the exported model. As a result, a **stylized_image.jpg** will be created in the **src** folder. You can use **scp** to download the generated image to your local computer and open the file locally to view the image.

1. Type `exit` in the console to close the ssh connection. At this point we are done with the training process and we can continue to the next section where we will be using the model in a real app.


## Run predictions locally using ML.NET

For this section you will use your local development environment to run the model from Visual Studio Code using a simple pipeline.

### A) Download the lab materials

Follow these steps to download the sample code provided for this lab. It includes a prebuilt React-based web application that captures an image and then sends the image content to an API for inferencing.

1. Click **Clone or download** from this repo.
1. You can clone the repo using **git** or click **Download ZIP** to directly download the code from your browser.
   > ALERT: make sure to uncompress/clone your code into **Downloads\styletransfer-lab**.

### B) Download the model

After the training is complete, the model can be downloaded to your local computer by using the **scp** command.

1. Return to your command prompt.
1. Move to your lab directory: `cd <your home dir>\Downloads\styletransfer-lab`
1. Execute the following command to copy the model: `scp -r <username>@<vm public ip address>:/home/<username>/styletransfer-lab/Training/StyleTransferTraining/src/export/* .\WebApp\StyleTransfer.Web\Models\`
    > NOTE: the **-r** stands for recursive, as you'll be copying the folder and all of its contents.

1. Provide your **password** when prompted.
    > NOTE: this process might take a few minutes.

1. Check the downloaded files by executing `dir .\WebApp\StyleTransfer.Web\Models\`. The folder should contain the `saved_model.pb` file and a `variables` folder.

### C) Open and set up your code in Visual Studio

Open your code in Visual Studio and install any missing dependency. The code provided should be compiling without errors.

1. Open `Visual Studio Code` from the **Start Menu**.
1. Go to **File -> Open Folder** and open the folder at `Downloads\styletransfer-lab\WebApp\StyleTransfer.Web`.
1. Wait for the code to load:
    * VS Code will prompt with an information dialog about unresolved dependencies. Click **Restore** to execute the restore command.
    * VS Code might prompt about missing required assets (C# Extension). Click **Yes** to add it.
1. Wait for VS Code to finish installing any missing package and loading your code.
1. Open the extensions menu (Ctrl+Shift+X) and search for `NuGet Package Manager`.
1. Click **Install** and restart VS Code.
1. Go to **View -> Command Palette** from VS Code Menu.
1. Type `NuGet` and select the first result **Add Package**.
1. Type `Microsoft.ML.TensorFlow` and press **Enter**.
   * Select the first result.
   * Select the version `0.11.0`.
   * Press **Enter**.

    > NOTE: that the main **ML.NET** package `Microsoft.ML` and `Microsoft.ML.ImageAnalytics` are already installed in the project as our code base includes a few methods that require those libraries.

### D) Write code to get predictions using a simple pipeline

The code base comes with a pre-built Web App and an API that applies the model to images sent from the Web App to the API. We'll add the code to use our model and run the predictions using ML.NET.

1. Open the `Controllers\StyleTransferController.cs` class from **VS Code**.
1. Review the `Post` method. It receives an image in *base64* format (from the web app) and then calls the `RunPrediction` method to send this image through our model and get a prediction. We'll be implementing this method in the next step.
1. Open the `Predictor.cs` file.
1. Find the method `RunPrediction` and modify the following lines:
    * Paste the following code snippet after the first comment `Prepare input data`:

    ```csharp
    var resizedImage = ImageUtils.ResizeImage(base64Image);
    var pixels = ImageUtils.ExtractPixels(resizedImage);
    var data = new[] { new TensorInput { Placeholder = pixels } };
    var dataView = _mlContext.Data.LoadFromEnumerable(data);
    ```

    > NOTE: here we resize the image to a specific size and extract the pixels. We use those pixels as the input data.

    * Paste the following code snippet after the comment `Create pipeline to execute our model`:

    ```csharp
    var pipeline = _mlContext.Transforms.ScoreTensorFlowModel(ImageConstants.ModelLocation, new[] { "add_37" }, new[] { "Placeholder" });
    ```

    > NOTE: this pipeline is only composed by a **TensorFlowEstimator**. This is just enough to get a prediction from our TensorFlow model. Notice the *input* and *output* columns are explicitly specified. You can get that info by opening the saved model in a tool like [Netron](https://github.com/lutzroeder/Netron).

    * Replace the last line `return null` with the following code snippet:

    ```csharp
    // Put the data in the pipeline to get a Transformer
    var model = pipeline.Fit(dataView);

    // Execute prediction
    var predictionsEngine = model.CreatePredictionEngine<TensorInput, TensorOutput>(_mlContext);
    var results = predictionsEngine.Predict(data[0]);

    return ProcessResult(results);
    ```
    > NOTE: here we apply the new style to the input pixels and return the transformed pixels in the prediction result.

1. Save your changes.

### E) Test your model

Let's run the pre-built front-end to test our model from Visual Studio Code.

1. In order to run the project you can press the **F5** key or you can click on **Debug -> Start Debugging** from VS Code menu.
   > NOTE: this starts both the .NET Core dev server, as well as the client Node.js development server. It might take a few minutes to start.

1. A new web page with the Web App will be opened in your browser. You'll see the *Style Transfer* page with a **Create!** button.
1. Click the **Create!** button, the app will take you to the *camera/picture* upload section.
   > NOTE: click **Allow** if the browser requests for permission to access you camera.

1. Click on **Capture** to take a picture and wait for a few seconds while the API runs the prediction. You'll see the image you uploaded with the new style applied in the right panel.
   > NOTE: you can take a picture of yourself or upload one from your PC.

## Build an Advanced Pipeline

See how to get predictions from the previous models to transform the images using an advanced pipeline.

### A) Write the code to build an advanced pipeline

Let's update the Prediction method to use a more complex pipeline. ML.NET has a range of components that can make working with data easier.

1. Return to **VS Code** and click on the **Stop** button (Shift+F5).
1. Open the `Predictor.cs` file.
1. Find the method `RunPrediction` and modify the following lines:
    * Replace the *4 lines* of code below the comment `Prepare input data` with the following code snippet:

    ```csharp
    var testImage = ImageUtils.SaveImageForPrediction(base64Image);
    var data = new[] { new ImageInput { ImagePath = testImage } };
    var dataView = _mlContext.Data.LoadFromEnumerable(data);
    ```
    > NOTE: here we save the image in a local directory and use the path as our input data.

    * Replace the line of code after the comment `Create pipeline to execute our model` with the following code snippet:

    ```csharp
    var pipeline = _mlContext.Transforms.LoadImages(imageFolder: ImageConstants.ImagesLocation, ("ImageObject", "ImagePath"))
                .Append(_mlContext.Transforms.ResizeImages(outputColumnName: "ImageObject", imageWidth: ImageConstants.ImageWidth, imageHeight: ImageConstants.ImageHeight, inputColumnName: "ImageObject"))
                .Append(_mlContext.Transforms.ExtractPixels("Placeholder", "ImageObject", interleave: true, offset: 0))
                .Append(_mlContext.Transforms.ScoreTensorFlowModel(modelLocation: ImageConstants.ModelLocation, inputColumnNames: new[] { "Placeholder" }, outputColumnNames: new[] { "add_37" }));
    ```

    > NOTE: this pipeline is chaining multiple estimators to transform the image: one estimator to load the images from a local path, another estimator to resize the image and one to extract the pixels. Finally, the last estimator from the chain executes the scoring using the tensor flow model. In this scenario the pipeline will perform all the required transformations on the image.

    * Replace the line of code after the comment `Execute prediction` with the following code snippet:

    ```csharp
    var predictionsEngine = model.CreatePredictionEngine<ImageInput, TensorOutput>(_mlContext);
    ```
    > NOTE: here we are only changing the type of the input parameter to accept our new *ImageInput* with the path.

1. Save your changes.
1. Run the Web App by pressing the **F5** key.
1. Follow the same steps as before to take a picture of yourself and test our new pipeline.
1. You'll see the image you uploaded with the new style applied in the right panel, this time using a more advanced transformation technique.

   > NOTE: in order to avoid saving the input image to disk, you can also try building your own [ImageLoadingTransformer](https://github.com/dotnet/machinelearning/blob/master/src/Microsoft.ML.ImageAnalytics/ImageLoader.cs) that ingests the images in memory instead of writing to disk.

*Media Elements and Templates. You may copy and use images, clip art, animations, sounds, music, shapes, video clips and templates provided with the sample application and identified for such use in documents and projects that you create using the sample application. These use rights only apply to your use of the sample application and you may not redistribute such media otherwise.*