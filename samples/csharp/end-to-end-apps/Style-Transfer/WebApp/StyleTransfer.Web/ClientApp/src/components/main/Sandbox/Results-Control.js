import React, {Component} from 'react';
import ImagePreview from '../../../assets/images/capture-background-8651085de3.png';
import PaletteImage from '../../../assets/images/paint-palette-d8588fef94.svg';
import './Results-Control.css';

export class ResultsControl extends Component {

  updateFilter = e => {
    this.props.updateFilter(e.currentTarget.value);
  };

  render() {
    const {
      filterOptions,
      filter: selectedFilter,
      processedImage,
      processingImage
    } = this.props;
    return (
        <div className="control-container">
          <form>
            <div className="styles">
              {filterOptions.map((filter, index) => {
                return (
                    <span key={index} className="style">
                  <input
                      type="radio"
                      id={filter}
                      name="style"
                      value={filter}
                      checked={filter === selectedFilter}
                      disabled={index > 0}
                      onChange={this.updateFilter}
                      className="invisible"
                  />
                  <label htmlFor={filter} className="style-label"><img src={PaletteImage} alt="" className={index > 0 ? 'disabled-image': ''}/>{filter}</label>
                </span>
                )
              })}
            </div>
          </form>
          <div className="wrapper">
            {!processingImage ? (
                <div className={'processed-image_container'}>
                  <img src={processedImage ? processedImage.base64Image : ImagePreview} alt=""/>
                </div>
            ) : (
                <div className="loader"></div>
            )}
          </div>
          <p className="right-link">
            <a href="https://www.ailab.microsoft.com/experiments/99907c05-d487-450b-9ee9-901b40205e81">Learn More on AI
              Lab</a>
          </p>
        </div>
    );
  }
}