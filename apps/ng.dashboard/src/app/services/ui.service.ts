import {Component, Injectable} from '@angular/core';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';
import {UiAction} from '../models/ui-action';

@Injectable()
export class UiService {
  private actionSource = new BehaviorSubject<UiAction>(null);
  currentAction = this.actionSource.asObservable();

  constructor() { }

  toggleSideMenu(): any {
    const action: UiAction = {target: 'SideMenuComponent', action: 'toggle'};
    return this.actionSource.next(action);
  }

  toggleNotification(): any {
    const action: UiAction = {target: 'NotificationCenter', action: 'toggle'};
    return this.actionSource.next(action);
  }
}
