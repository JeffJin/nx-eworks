import {Injectable, NgZone} from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {HttpClient} from '@angular/common/http';
import {CacheService} from './cache.service';
import {Router} from '@angular/router';
import {environment} from '@environments/environment';
import {map} from 'rxjs/operators';

@Injectable()
export class UserService {

  constructor(private http: HttpClient) {
  }

  validateEmail(email: string): Observable<{ [key: string]: any }> {
    console.log('validating email through backend api', email);
    return this.http.get<{ [key: string]: any }>(`${environment.apiBaseUrl}/users/validate_email?email=${email}`)
      .pipe(map(result => {
        if (!result) {
          return null;
        }
        return {[result.key]: result.value};
      }));
  }

  validateUserName(userName: string): Observable<{ [key: string]: any }> {
    console.log('validating user name through backend api', userName);
    return this.http.get<{ [key: string]: any }>(`${environment.apiBaseUrl}/users/validate_username?userName=${userName}`)
      .pipe(map(result => {
        if (!result) {
          return null;
        }
        return {[result.key]: result.value};
      }));
  }
}
