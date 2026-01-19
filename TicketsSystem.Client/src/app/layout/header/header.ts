import { Component, OnInit } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Router, RouterModule } from '@angular/router';
import { DarkModeService } from '../../core/services/dark-mode.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  imports: [ButtonModule, RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {

  constructor(private darkModeService: DarkModeService, private authService: AuthService, private router: Router) { }

  toogleDarkMode(): void {
    this.darkModeService.toogleDarkMode();
  }

  darkModeEnabled(): boolean {
    return this.darkModeService.darkModeEnabled();
  }

  login(): void {
    this.authService.login();
    this.router.navigate(['/main']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }
}
