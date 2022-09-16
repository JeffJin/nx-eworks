import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MergeImageComponent } from './merge-image.component';

describe('MergeImageDialogComponent', () => {
  let component: MergeImageComponent;
  let fixture: ComponentFixture<MergeImageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MergeImageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MergeImageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
