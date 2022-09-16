import {Component, OnDestroy, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';
import {UntypedFormBuilder, UntypedFormGroup} from '@angular/forms';
import {UiService} from '@app/services/ui.service';
import {Notification} from '@app/models/notification';
import {SideMenuComponent} from './side-menu/side-menu.component';
import {NotificationComponent} from '@app/components/widgets/notification/notification.component';
import {AssetsState} from '@app/modules/dashboard/assets/states/assets.state';
import {Store} from '@ngrx/store';
import {MatSnackBar} from '@angular/material/snack-bar';
import {Subscription} from 'rxjs/Subscription';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild('leftMenu') leftMenu: SideMenuComponent;
  @ViewChild('ntfCenter') ntfCenter: NotificationComponent;

  ready = false;
  leftMenuOpen = false;
  options: UntypedFormGroup;
  events: Array<Notification> = [];
  private errorSubscriber: Subscription;

  constructor(private store: Store<AssetsState>,
              private fb: UntypedFormBuilder,
              public snackBar: MatSnackBar,
              private uiService: UiService) {
    this.options = this.fb.group({
      fixed: false,
      top: 0,
      bottom: 0,
    });
    this.ready = true;
  }

  ngOnDestroy(): void {
    this.errorSubscriber.unsubscribe();
  }

  ngOnInit(): void {
    this.uiService.currentAction.subscribe(next => {
      if (next && next.target === 'SideMenuComponent') {
        this.leftMenuOpen = !this.leftMenuOpen;
      }
      if (next && next.target === 'NotificationCenter') {
        // this[next.action]();
        this.ntfCenter.toggle();
      }
    });
  }

}
