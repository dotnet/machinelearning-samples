import React, { Component } from 'react';
import Header from './components/header';
import Main from './components/main';
import Footer from './components/footer';
import './Layout.css';

export class Layout extends Component {

  render() {
    return (
      <div className="layout">
        <Header></Header>
        <Main></Main>
        <Footer></Footer>
      </div>
    );
  }
}
