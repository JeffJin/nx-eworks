import { Injectable } from '@angular/core';
import {HubConnectionBuilder} from '@aspnet/signalr';

@Injectable()
export class SignalrService {

  constructor() { }

  getHubConnectionBuilder() {
    return new HubConnectionBuilder();
  }
}
