import { Component, Input } from '@angular/core';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { Button } from "primeng/button";
import { Router } from "@angular/router";
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [CardModule, InputTextModule, Button],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {

  constructor(private router: Router, private authService: AuthService) {}
  
  login(): void {
    this.authService.login();
    this.router.navigate(['/main']);
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

}
