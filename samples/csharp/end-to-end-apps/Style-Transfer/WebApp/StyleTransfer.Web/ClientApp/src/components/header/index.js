import React, { Component } from 'react';
import MsLogo from '../../assets/images/ms-logo.png';
import AiLogo from '../../assets/images/ai-logo.png';
import './Header.css';

export default class Header extends Component {

  render() {
    return (
      <header className="header">
        <a href="https://www.microsoft.com">
          <img src={MsLogo} alt="Microsoft homepage" className="logo" />
        </a>
        <a href="https://www.microsoft.com/en-us/ai">
          <img src={AiLogo} alt="Microsoft AI homepage" className="ai-logo" />
        </a>
      </header>
    );
  }
}
