import {Component, Input, OnInit, ViewEncapsulation} from '@angular/core';
import {ProgressItem} from '../../models/progress-item';

@Component({
  selector: 'app-progress',
  templateUrl: './progress.component.html',
  styleUrls: ['./progress.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class ProgressComponent implements OnInit {

  constructor() {
  }

  @Input() progressItem: ProgressItem;

  ngOnInit(): void {
  }

}
