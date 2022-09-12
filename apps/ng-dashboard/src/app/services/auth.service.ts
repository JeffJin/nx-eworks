import {Injectable, NgZone} from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {HttpClient, HttpHeaders, HttpRequest} from '@angular/common/http';
import {CacheService} from './cache.service';
import { environment } from '@environments/environment';
import {Router} from '@angular/router';
import {mockData} from './mock-data';
import StatusResponse = facebook.StatusResponse;
import AuthResponse = facebook.AuthResponse;

@Injectable()
export class AuthService {

  constructor(private http: HttpClient, private zone: NgZone, private router: Router, private cacheService: CacheService) { }

  login(email: string, password: string): Observable<any> {
    const dto = {email, password};
    return this.http.post(`${environment.apiBaseUrl}/account/login`,
          dto, {headers : new HttpHeaders({ 'Content-Type': 'application/json' })});
  }

  externalLogin(provider: string): Observable<any> {
    return new Observable<any>(observer => {
      if (provider === 'facebook') {
        FB.login((response: StatusResponse) => {
          this.zone.run(() => {
            if (response.status === 'connected') {
              const auth: AuthResponse = response.authResponse;
              console.log('facebook login for: ', auth);

              // 1.validate the token
              // 2.register or update the user
              // 3.set application token
              FB.api('/me',
                'get',
                {fields: 'id,name,email'}, (res: any) => {
                console.log('Successful login for: ', res);
                const fbLoginDto = {
                  userID: auth.userID,
                  userName: res.name,
                  email: res.email,
                  accessToken: auth.accessToken,
                };

                this.http.post(
                  `${environment.apiBaseUrl}/account/fb_login`,
                  fbLoginDto,
                  {headers : new HttpHeaders({ 'Content-Type': 'application/json' })}
                ).subscribe((data: any) => {
                  // store the result into local storage
                  observer.next(data);
                  observer.complete();
                }, (err) => {
                  observer.error(err);
                });

              });

            } else {
              observer.error();
            }
          });
        }, {scope: 'public_profile,email'});
      } else {
        throw new Error('unsupported provider ' + provider);
      }
    });
  }

  register(userName: string, email: string, password: string, confirmPassword: string): Observable<any> {
    if (password !== confirmPassword){
      return new Observable(obs => {
        obs.next({message: 'Passwords do not match', code: 'PasswordsNotMatch'});
        obs.complete();
      });
    }
    const formData: FormData = new FormData();
    formData.append('userName', userName);
    formData.append('email', email);
    formData.append('confirmPassword', confirmPassword);
    formData.append('password', password);
    return this.http.post(environment.apiBaseUrl + '/account/register', formData);
  }

  getCurrentUser(): Observable<any>{
    return new Observable(observer => {
      if (environment.noBackend) {
        observer.next(mockData.user);
        observer.complete();
      } else {
        this.http.get(environment.apiBaseUrl + '/user').subscribe((data: any) => {
          this.cacheService.setUser(data);
          observer.next(data);
          observer.complete();
        }, (err) => {
          // observer.error(err);
          return this.router.navigateByUrl('login');
        });
      }


    });
  }

  initXsrfToken(): Observable<any> {
    const token = this.cacheService.getXsrfToken();
    if (token && token.tokenName) {
      return new Observable((obs) => {
        obs.next(token);
        obs.complete();
      });
    }
    else {
      this.http.get(environment.apiBaseUrl + '/common/xsrf').subscribe((data: any) => {
        // store the result into local storage
        this.cacheService.setXsrfToken(data);
      }, (err) => {
        this.cacheService.setXsrfToken(null);
      });
    }
  }

  logout(): Observable<any>{
    return this.http.post(environment.apiBaseUrl + '/account/logout', null);
  }

  confirmEmail(userId: string, code: string): Observable<any> {
    const formData: FormData = new FormData();
    formData.append('UserId', userId);
    formData.append('Code', code);
    return this.http.post(environment.apiBaseUrl + '/account/confirm_email', formData);
  }
}
