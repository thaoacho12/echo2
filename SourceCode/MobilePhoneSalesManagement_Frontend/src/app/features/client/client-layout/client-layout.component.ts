import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from "../../../core/components/admin/navbar/navbar.component";
import { HeaderComponent } from '../../../core/components/common/header/header.component';
import { Component, OnInit, Renderer2 } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FooterComponent } from "../../../core/components/common/footer/footer.component";

@Component({
  selector: 'app-client-layout',
  imports: [RouterOutlet, HeaderComponent, FooterComponent],
  templateUrl: './client-layout.component.html',
  styleUrl: './client-layout.component.css',
  standalone: true,
})



export class ClientLayoutComponent implements OnInit {

  constructor(private renderer: Renderer2, private route: ActivatedRoute) {}

  ngOnInit(): void {
  }

}
