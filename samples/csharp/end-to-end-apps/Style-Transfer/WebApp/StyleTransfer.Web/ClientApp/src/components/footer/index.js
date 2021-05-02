import React, { Component } from 'react';
import './Footer.css';

export default class Footer extends Component {

  render() {
    return (
      <footer className="footer">
        <span>Your photo, but no personal info, may be used to improve the underlying model.</span>
        <div className="footer-links">
          <a href="https://support.microsoft.com/en-us/contactus">Contact us</a>
          <a href="https://go.microsoft.com/fwlink/?LinkId=521839">Privacy &amp; cookies</a>
          <a href="https://go.microsoft.com/fwlink/?LinkID=206977">Terms of use</a>
          <a href="https://www.microsoft.com/trademarks">Trademarks</a> 
          <span>© Microsoft 2018</span>
        </div>
      </footer>
    );
  }
}
