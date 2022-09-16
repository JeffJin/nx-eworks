import {ChangeDetectionStrategy, Component, EventEmitter, Inject, Input, OnDestroy, OnInit, Output, ViewEncapsulation} from '@angular/core';
import {CategoryDto} from '@app/models/dtos';

@Component({
  selector: 'app-categories',
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.scss'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesComponent {
  @Input() categories: CategoryDto[];
  @Input() baseRoute: string;

  constructor(
  ) {
  }
}
