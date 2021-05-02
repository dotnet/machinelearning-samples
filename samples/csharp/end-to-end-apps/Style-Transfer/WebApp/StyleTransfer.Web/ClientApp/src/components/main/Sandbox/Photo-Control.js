import React, { Component } from 'react';
import ImagePreview from '../../../assets/images/capture-computer-background-37de2649d6.png';
import './Photo-Control.css';

export class PhotoControl extends Component {

  MAX_CHAR_LENGTH = 18;

  state = {
    videoDevices: [],
    value: 'Select Camera',
    displayUploadContainer: false,
    fileName: '',
    uploadedImagePreview: ''
  };

  componentDidMount() {
    this.initCamera();
  }

  initCamera = () => {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
      navigator.mediaDevices.getUserMedia({ video: true })
        .then(stream => {
          try {
            this.videoRef.srcObject = stream;
          } catch (error) {
            this.videoRef.src = URL.createObjectURL(stream);
          }
          this.setVideoDevices();
          this.videoRef.play();
        });
    }
  }

  setVideoDevices = () => {
    navigator.mediaDevices.enumerateDevices()
      .then(devices => {
        const videoDevices = devices.filter(device => device.kind === 'videoinput');
        this.setState({
          video: this.videoRef,
          videoDevices
        });
      });
  };

  handleChange = event => {
    this.setState({
      value: event.target.value
    });
  };

  toggleUploadContainer = displayUploadContainer => {
    this.setState({
      displayUploadContainer
    }, () => {
      !this.state.displayUploadContainer && this.initCamera();
    });
  };

  captureImage = () => {
    const {displayUploadContainer, uploadedImagePreview} = this.state;
    if (displayUploadContainer) {
      // Upload Image
      this.props.processPhoto(uploadedImagePreview);
    } else {
      // Taken with camera
      const width = this.videoRef.videoWidth;
      const height = this.videoRef.videoHeight;
      var context = this.canvasRef.getContext('2d');
      this.canvasRef.width = width;
      this.canvasRef.height = height;
      context.drawImage(this.videoRef, 0, 0, width, height);
      const data = this.canvasRef.toDataURL('image/png');
      this.props.processPhoto(data);
    }
  };

  showUploadedImage = e => {

    if (e.target.files && e.target.files[0]) {
      const reader = new FileReader();
      const that = this;
      reader.onload = function (ev) {
        that.setState({ uploadedImagePreview: ev.target.result});
      };

      this.setState({ fileName: e.target.files[0].name });
      reader.readAsDataURL(e.target.files[0]);
    }

  };

  render() {
    const {
      videoDevices,
      displayUploadContainer,
      uploadedImagePreview,
      fileName
    } = this.state;

    const btnClass = 'selector-btn'
    const btnClassActive = 'selector-btn active'

    return (
      <div className="control-container">
        <div className="mode-selector">
          <a className={displayUploadContainer ? btnClass : btnClassActive} onClick={() => this.toggleUploadContainer(false)}>Camera</a>
          <a className={!displayUploadContainer ? btnClass : btnClassActive} onClick={() => this.toggleUploadContainer(true)}>Upload a picture</a>
        </div>
        {displayUploadContainer ? (
          <div className="upload-container">
            <input type="file" id="button-upload" className="hidden" onChange={this.showUploadedImage}/> 
            <label htmlFor="button-upload">Upload a picture</label>
            <label htmlFor="file"><span>{fileName}</span></label>
            <div className="upload-container_image">
              <img ref={imgRef => this.imgRef = imgRef} src={uploadedImagePreview.length > 0 ? uploadedImagePreview : ImagePreview} alt="" />
            </div>
          </div>
        ) : (
            <div className="option">
              {videoDevices.length > 0 && 
                <select value={this.state.value} onChange={this.handleChange}>
                  {videoDevices.map(videoDevice => (
                    <option key={videoDevice.deviceId} value={videoDevice.deviceId}>{videoDevice.label.substring(0, this.MAX_CHAR_LENGTH)}</option>
                  ))}
                </select>
              }
            </div>
        )}
        {!displayUploadContainer && <video className="stream" ref={vidRef => this.videoRef = vidRef}></video>}
        <canvas ref={canvasRef => this.canvasRef = canvasRef} className="screenshot hidden"/>
        <div className="hidden invisible modal-section">
          <div className="modal-subsection modal-content-camera_access">
          <p className="title">The browser cannot access your camera <span className="alert" id="alert-webRTC"></span></p>
          <p>Check your camera's connections, its settings and permissions or try to select another camera or browser.</p>
          </div>
        </div>
        <button 
          className="button"
          disabled={displayUploadContainer && uploadedImagePreview.length === 0}
          onClick={this.captureImage}
        >
          {displayUploadContainer?"Upload":"Capture"}
        </button>
      </div>
    );
  }
}