import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MergeAudioComponent } from './merge-audio.component';

describe('MergeAudioDialogComponent', () => {
  let component: MergeAudioComponent;
  let fixture: ComponentFixture<MergeAudioComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MergeAudioComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MergeAudioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
