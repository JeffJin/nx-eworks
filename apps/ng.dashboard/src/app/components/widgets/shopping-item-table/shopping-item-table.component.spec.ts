import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingItemTableComponent } from './shopping-item-table.component';

describe('ShoppingItemTableComponent', () => {
  let component: ShoppingItemTableComponent;
  let fixture: ComponentFixture<ShoppingItemTableComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ShoppingItemTableComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ShoppingItemTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
