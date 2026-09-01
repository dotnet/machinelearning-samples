import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './Layout';
import './App.css'

export default class App extends Component {
  displayName = App.name

  render() {
    return (
      <Route exact path='/' component={Layout} />
    );
  }
}
