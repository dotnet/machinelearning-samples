# Welcome!

If you are here, it means you are interested in helping us out. A hearty welcome and thank you! There are many ways you can contribute to the ML.NET Samples:

* Offer PR's to fix bugs.
* Give us feedback and bug reports regarding the samples or the documentation.
* Improve our samples, tutorials, and documentation.

## Getting started:

Please join the community on Gitter [![Join the chat at https://gitter.im/dotnet/mlnet](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/mlnet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge). Also please make sure to take a look at the samples [roadmap](ROADMAP.md).

### Pull requests

If you are new to GitHub [here](https://help.github.com/categories/collaborating-with-issues-and-pull-requests/) is a detailed help source on getting involved with development on GitHub.

As a first time contributor, you will be invited to sign the Contributor License Agreement (CLA). Please follow the instructions of the .NET foundation bot reviewer on your PR to sign the agreement indicating that you have appropriate rights to your contribution.

Your pull request needs to reference a filed issue. Please fill in the template that is populated for the pull request. Only pull requests addressing small typos can have no issues associated with them.

An ML.NET team member will be assigned to your pull request once the continuous integration checks have passed successfully.

All commits in a pull request will be squashed to a single commit with the original creator as author.


## Uploading datasets
* Only datasets that allowed for public use for all purposes (including redistribution) can be uploaded to this repository. 
* To avoid the repository growing too large that it's not convenient to work with, the limit for an uploaded dataset file is 5 MB. Everything that is bigger should be downloaded programmatically on the first run of the app.
* All datasets should be stored in [datasets](https://github.com/dotnet/machinelearning-samples/tree/master/datasets) folder to allow reusing them by other examples.
* If you are uploading a dataset, please add a section in datasets [README](datasets/README.md) file describing the original source and license.
