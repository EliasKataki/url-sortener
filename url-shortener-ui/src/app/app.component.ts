import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  isHome = true;
  constructor(private router: Router) {
    this.router.events.subscribe(() => {
      this.isHome = this.router.url === '/' || this.router.url === '/home';
    });
  }
}
