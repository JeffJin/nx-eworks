import { TestBed, inject } from '@angular/core/testing';

import { MessageQueueService } from './message-queue.service';

describe('MessageQueueService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MessageQueueService]
    });
  });

  it('should be created', inject([MessageQueueService], (service: MessageQueueService) => {
    expect(service).toBeTruthy();
  }));
});
