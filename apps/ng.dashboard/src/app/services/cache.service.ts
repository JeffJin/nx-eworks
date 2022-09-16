import { Injectable } from '@angular/core';
import {UserDto} from '../models/dtos';

@Injectable()
export class CacheService {

  constructor() { }

  getToken(): string {
    const token = localStorage.getItem('jtw');
    if (token) {
      return JSON.parse(token);
    }
    return null;
  }

  setToken(token: string): void {
    if (!token) {
      localStorage.removeItem('jtw');
      return;
    }
    localStorage.setItem('jtw', JSON.stringify(token));
  }


  getUser(): any {
    try{
      const item = localStorage.getItem('user');
      return JSON.parse(item);
    }
    catch (err) {
      return null;
    }
  }

  setUser(user: any): void {
    if (!user) {
      localStorage.removeItem('user');
      return;
    }
    const dto: UserDto = {
      userName: user.userName,
      email: user.email,
      phoneNumber: user.phoneNumber
    };
    const item = JSON.stringify(dto);
    localStorage.setItem('user', item);
  }

  setXsrfToken(token: any): void {
    localStorage.setItem('xsrf', token);
  }

  getXsrfToken(): any {
    return localStorage.getItem('xsrf');
  }
}
