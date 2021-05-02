import React, { Component } from 'react';
import './Welcome.css';

export class Welcome extends Component {

  onCreate = () => {
    this.props.displaySanbox();
  }

  render() {
    return (
      <div className="step welcome">
        <div className="intro">
          <h1>Style Transfer</h1>
          <p className="subtitle">Creating Art with Artificial Intelligence</p>
          <p>Look how our AI creates a reflection of you using an artistic effect of your selection.</p>
          <button className="button" onClick={this.onCreate}>Create!</button>
        </div>
      </div>
    );
  }
}