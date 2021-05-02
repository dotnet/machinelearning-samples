import React, {Component} from 'react';
import {PhotoControl} from './Photo-Control';
import {ResultsControl} from './Results-Control';
import {getProcessedImage} from '../../../services/image.api';
import './Sandbox.css';

export class Sandbox extends Component {

  filterOptions = ['Candy', 'Feathers', 'Mosaic', 'Robert'];

  state = {
    filter: this.filterOptions[0],
    processingImage: false,
    processedImage: null,
  };

  onProcessPhoto = data => {
    const {filter} = this.state;
    this.setState({processingImage: true});
    getProcessedImage({filter, data})
        .then(res => {
          this.setState({
            processedImage: res.data,
            processingImage: false
          });
        })
        .catch(error => {
          console.log(error);
        });
  };

  onUpdateFilter = filter => {
    this.setState({filter});
  };

  render() {
    const {filter, processedImage, processingImage} = this.state;
    return (
        <div className="step sandbox">
          <PhotoControl
              processPhoto={this.onProcessPhoto}
          />
          <ResultsControl
              filterOptions={this.filterOptions}
              filter={filter}
              updateFilter={this.onUpdateFilter}
              processedImage={processedImage}
              processingImage={processingImage}
          />
        </div>
    );
  }
}