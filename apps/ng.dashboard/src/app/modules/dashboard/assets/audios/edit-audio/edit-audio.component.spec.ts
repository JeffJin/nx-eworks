import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditAudioComponent } from './edit-audio.component';

describe('EditAudioComponent', () => {
  let component: EditAudioComponent;
  let fixture: ComponentFixture<EditAudioComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditAudioComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditAudioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
