<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 350">
  <!-- Definizione delle animazioni -->
  <defs>
    <style>
      @keyframes rotate-inner {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
      }
      
      @keyframes rotate-outer {
        from { transform: rotate(0deg); }
        to { transform: rotate(-360deg); }
      }
      
      .inner-circle {
        transform-origin: 400px 175px; 
        animation: rotate-inner 20s linear infinite;
      }
      
      .outer-circle {
        transform-origin: 400px 175px;
        animation: rotate-outer 30s linear infinite;
      }
    </style>
  </defs> 
  
  <!-- Sfondo -->
  <rect width="800" height="350" fill="white" />
  
  <!-- Cerchio principale (logo) -->
  <circle cx="400" cy="175" r="100" fill="#3498db" opacity="0.9" />
  <text x="400" y="175" font-family="Arial, sans-serif" font-size="24" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="white">HubConnect</text>
  
  <!-- Cerchi che ruotano -->
  <circle class="inner-circle" cx="400" cy="175" r="115" fill="transparent" stroke="#3498db" stroke-width="2" stroke-dasharray="10,5" />
  <circle class="outer-circle" cx="400" cy="175" r="130" fill="transparent" stroke="#3498db" stroke-width="1" stroke-dasharray="5,5" />
</svg>