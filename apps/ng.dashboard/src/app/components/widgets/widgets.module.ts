import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {SimpleTableComponent} from '@app/components/widgets/simple-table/simple-table.component';
import {ActionTableComponent} from '@app/components/widgets/action-table/action-table.component';
import {ShoppingItemTableComponent} from '@app/components/widgets/shopping-item-table/shopping-item-table.component';
import {FormsModule} from '@angular/forms';
import {VideoPlayerComponent} from '@app/components/widgets/video-player/video-player.component';
import {NotificationComponent} from '@app/components/widgets/notification/notification.component';
import {TopNavComponent} from '@app/components/widgets/top-nav/top-nav.component';
import {AdworksMaterialModule} from '@app/adworks-material.module';
import {RouterModule} from '@angular/router';

@NgModule({
  declarations: [
    SimpleTableComponent,
    ActionTableComponent,
    ShoppingItemTableComponent,
    VideoPlayerComponent,
    NotificationComponent,
    TopNavComponent,
  ],
  exports: [
    SimpleTableComponent,
    ActionTableComponent,
    VideoPlayerComponent,
    NotificationComponent,
    TopNavComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    AdworksMaterialModule,
    RouterModule,
  ]
})
export class WidgetsModule { }
