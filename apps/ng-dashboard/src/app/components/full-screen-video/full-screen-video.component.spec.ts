import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FullScreenVideoComponent } from './full-screen-video.component';

describe('FullScreenVideoComponent', () => {
  let component: FullScreenVideoComponent;
  let fixture: ComponentFixture<FullScreenVideoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FullScreenVideoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FullScreenVideoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
