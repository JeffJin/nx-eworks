import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {Store} from '@ngrx/store';
import {loadCategories} from '@app/modules/dashboard/assets/states/assets.actions';
import {AssetsState} from '@app/modules/dashboard/assets/states/assets.state';

@Component({
  selector: 'app-assets',
  templateUrl: './assets.component.html',
  styleUrls: ['./assets.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AssetsComponent implements OnInit {

  constructor(private store: Store) { }

  ngOnInit(): void{
    this.store.dispatch(loadCategories());
  }

}
