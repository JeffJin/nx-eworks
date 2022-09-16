import { Injectable } from '@angular/core';
import {CacheService} from './cache.service';

@Injectable()
export class MessageQueueService {

  constructor(private cacheService: CacheService) {

  }

}
