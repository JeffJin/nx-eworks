import { Component, OnInit, ElementRef} from '@angular/core';
import {Router, NavigationEnd} from '@angular/router';
import { Subscription } from 'rxjs/Subscription';

@Component({
    selector: 'app-navbar',
    templateUrl: 'navbar.component.html'
})
export class NavbarComponent implements OnInit {
  private toggleButton: any;
  private sidebarVisible: boolean;
  mobileMenuVisible: any = 0;
  private routerSub: Subscription;

  constructor(private router: Router, private element: ElementRef) {
    this.sidebarVisible = false;
  }
  ngOnInit(): void {
    const navbar: HTMLElement = this.element.nativeElement;
    this.toggleButton = navbar.getElementsByClassName('navbar-toggler')[0];
    this.routerSub = this.router.events.subscribe((event: any) => {
      if (event instanceof NavigationEnd) {
        this.sidebarClose();
        const $layer = document.getElementsByClassName('close-layer')[0];
        if ($layer) {
          $layer.remove();
        }
      }
    });
  }
  sidebarOpen(): void {
    const $toggle = document.getElementsByClassName('navbar-toggler')[0];
    const toggleButton = this.toggleButton;
    const body = document.getElementsByTagName('body')[0];
    setTimeout(() => {
      toggleButton.classList.add('toggled');
    }, 500);
    body.classList.add('nav-open');
    setTimeout(() => {
      $toggle.classList.add('toggled');
    }, 430);

    const $layer = document.createElement('div');
    $layer.setAttribute('class', 'close-layer');


    if (body.querySelectorAll('.wrapper-full-page')) {
      document.getElementsByClassName('wrapper-full-page')[0].appendChild($layer);
    }else if (body.classList.contains('off-canvas-sidebar')) {
      document.getElementsByClassName('wrapper-full-page')[0].appendChild($layer);
    }

    setTimeout(() => {
      $layer.classList.add('visible');
    }, 100);

    $layer.onclick = () => {
      body.classList.remove('nav-open');
      this.mobileMenuVisible = 0;
      this.sidebarVisible = false;

      $layer.classList.remove('visible');
      setTimeout(() => {
        $layer.remove();
        $toggle.classList.remove('toggled');
      }, 400);
    };

    body.classList.add('nav-open');
    this.mobileMenuVisible = 1;
    this.sidebarVisible = true;
  }
  sidebarClose(): void {
    const $toggle = document.getElementsByClassName('navbar-toggler')[0];
    const body = document.getElementsByTagName('body')[0];
    this.toggleButton.classList.remove('toggled');
    const $layer = document.createElement('div');
    $layer.setAttribute('class', 'close-layer');

    this.sidebarVisible = false;
    body.classList.remove('nav-open');
    // $('html').removeClass('nav-open');
    body.classList.remove('nav-open');
    if ($layer) {
      $layer.remove();
    }

    setTimeout(() => {
      $toggle.classList.remove('toggled');
    }, 400);

    this.mobileMenuVisible = 0;
  }
  sidebarToggle(): void {
    if (this.sidebarVisible === false) {
      this.sidebarOpen();
    } else {
      this.sidebarClose();
    }
  }
}
