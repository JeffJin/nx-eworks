import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {LoginComponent} from './components/login/login.component';
import {ConfirmEmailComponent} from './components/confirm-email/confirm-email.component';
import {PrivateGuard} from './guards/private.guard';
import {FullScreenVideoComponent} from './components/full-screen-video/full-screen-video.component';
import {ErrorComponent} from './components/error/error.component';
import {ServerErrorComponent} from './components/error/server-error/server-error.component';
import {NotAllowedComponent} from './components/error/not-allowed/not-allowed.component';
import {PageNotFoundComponent} from './components/page-not-found/page-not-found.component';
import {RegisterComponent} from './components/register/register.component';
import {PromotionComponent} from './components/promotion/promotion.component';
import {LiveStreamComponent} from './components/live-stream/live-stream.component';
import {StoreRouterConnectingModule} from '@ngrx/router-store';

const routes: Routes = [
  { path: '', component: LiveStreamComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'promotion', component: PromotionComponent },
  { path: 'confirm_email', component: ConfirmEmailComponent },
  { path: 'reset_password', component: ConfirmEmailComponent },
  { path: 'play', component: FullScreenVideoComponent },
  { path: 'error', component: ErrorComponent,
    children: [
      { path: '', redirectTo: '500', pathMatch: 'full' },
      { path: '0', component: ServerErrorComponent },
      { path: '401', component: NotAllowedComponent },
      { path: '500', component: ServerErrorComponent }
    ]
  },
  { path: 'dashboard', loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule) },
  { path: '**', component: PageNotFoundComponent },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, { enableTracing: false }),
    StoreRouterConnectingModule.forRoot(),
  ],
  exports: [RouterModule]
})
export class AppRoutingModule { }
