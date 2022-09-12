import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterDeviceDialogComponent } from './register-device.component';

describe('RegisterDeviceDialogComponent', () => {
  let component: RegisterDeviceDialogComponent;
  let fixture: ComponentFixture<RegisterDeviceDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RegisterDeviceDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RegisterDeviceDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
