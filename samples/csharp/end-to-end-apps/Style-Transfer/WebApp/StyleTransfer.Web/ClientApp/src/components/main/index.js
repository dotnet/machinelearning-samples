import React, { Component } from 'react';
import { Welcome } from './Welcome/Welcome.js';
import { Sandbox } from './Sandbox/Sandbox.js';
import './Main.css';

export default class Main extends Component {

  state = {
    shouldDisplaySandbox: false
  }

  displaySanbox = () => {
    this.setState({
      shouldDisplaySandbox: true
    });
  }

  render() {
    const { shouldDisplaySandbox } = this.state;
    return (
      <main className="main">
        <p id="alert-filesize" className="alert invisible hidden">
          Image being uploaded is bigger than 4MB. Please try a different image.
        </p>
        {shouldDisplaySandbox ? (
          <Sandbox />
        ) : (
          <Welcome displaySanbox={this.displaySanbox}/>
        )}
      </main>
    );
  }
}
