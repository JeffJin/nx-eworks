import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { NxModule } from '@nrwl/nx';
import { RouterModule } from '@angular/router';
import { authRoutes, SecurityModule } from '@eworks/security';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    NxModule.forRoot(),
    RouterModule.forRoot([{ path: 'auth', children: authRoutes }], { initialNavigation: "enabledNonBlocking" }),
    SecurityModule     // added
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
