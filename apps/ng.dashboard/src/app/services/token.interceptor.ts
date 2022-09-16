import {Injectable} from '@angular/core';
import {
  HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest,
  HttpResponse
} from '@angular/common/http';
import {Observable} from 'rxjs/Observable';
import {CacheService} from './cache.service';
import 'rxjs/add/operator/do';
import {Router} from '@angular/router';
import {EntityDto} from '../models/dtos';
import {tap} from 'rxjs/operators';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {

  constructor(public cacheSvc: CacheService, private router: Router) {
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.cacheSvc.getToken();
    const headers = {
      Authorization: ''
    };
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
    request = request.clone({
      setHeaders: headers
    });

    // if the request is POST or PUT, add auditing data into the payload
    if (request.method === 'POST') {
      this.updateCreatedBy(request);
    }
    if (request.method === 'PUT') {
      this.updateUpdatedBy(request);
    }

    return next.handle(request)
      .pipe(
        tap((event: HttpEvent<any>) => {
          if (event instanceof HttpResponse) {
          }
        }, (err: any) => {
          return err;
        })
      );
  }

  updateUpdatedBy(request: HttpRequest<EntityDto>): void {
    if (request.body) {
      request.body.update(this.cacheSvc.getUser() ? this.cacheSvc.getUser().email : '');
    }
  }

  updateCreatedBy(request: HttpRequest<EntityDto>): void {
    if (request.body) {
      request.body.createdBy = this.cacheSvc.getUser() ? this.cacheSvc.getUser().email : '';
      request.body.updatedBy = '';
      request.body.updatedOn = null;
      request.body.createdOn = new Date();
    }
  }

}
