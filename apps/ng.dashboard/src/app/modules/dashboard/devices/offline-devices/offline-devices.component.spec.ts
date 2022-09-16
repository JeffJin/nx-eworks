import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OfflineDevicesComponent } from './offline-devices.component';

describe('OfflineDevicesComponent', () => {
  let component: OfflineDevicesComponent;
  let fixture: ComponentFixture<OfflineDevicesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OfflineDevicesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OfflineDevicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
