import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import {CacheService} from '../services/cache.service';

@Injectable()
export class AdminGuard implements CanActivate {

  constructor(private cacheService: CacheService){

  }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {

    if(this.cacheService.getToken()){
      return true;
    }

    return false;
  }
}
