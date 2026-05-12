<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerificarQR.aspx.cs" Inherits="Monolito4bm.VerificarQR" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Verificar QR</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-direction: column;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: #fff;
      gap: 24px;
      padding: 32px 16px;
    }
    .card {
      background: #fff;
      color: #1a1a2e;
      border-radius: 16px;
      padding: 36px 32px;
      max-width: 480px;
      width: 100%;
      text-align: center;
      box-shadow: 0 20px 60px rgba(0,0,0,0.4);
    }
    .card h2 { margin-bottom: 10px; color: #0f3460; }
    .card p  { color: #555; font-size: .9rem; margin-bottom: 20px; }
    #visor {
      width: 100%;
      max-width: 360px;
      border-radius: 10px;
      overflow: hidden;
      margin: 0 auto 16px;
      position: relative;
      background: #000;
    }
    #visor video {
      width: 100%;
      display: block;
    }
    #visor canvas { display: none; }
    /* Marco animado */
    .scan-frame {
      position: absolute;
      inset: 0;
      border: 3px solid #0f3460;
      border-radius: 10px;
      pointer-events: none;
    }
    .scan-line {
      position: absolute;
      left: 0; right: 0;
      height: 2px;
      background: rgba(15,52,96,.7);
      animation: scan 2s linear infinite;
    }
    @keyframes scan { 0%{top:0} 100%{top:100%} }

    .estado {
      font-size: .88rem;
      padding: 8px 12px;
      border-radius: 6px;
      margin-bottom: 12px;
    }
    .estado.espera  { background:#eef2ff; color:#3b4fd8; }
    .estado.ok      { background:#e6f9ef; color:#1a7a3d; }
    .estado.error   { background:#fff0f0; color:#c0392b; }

    #btnActivar {
      padding: 10px 24px;
      background: #0f3460;
      color: #fff;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: .95rem;
      font-weight: 700;
      transition: background .2s;
    }
    #btnActivar:hover { background: #16213e; }

    /* Hidden form para postback */
    #frmOtp { display:none; }
  </style>
</head>
<body>
  <div class="card">
    <h2>📷 Verificación QR</h2>
    <p>Revisa tu correo electrónico y escanea el código QR con la cámara de tu computadora.</p>

    <div id="visor">
      <video id="video" autoplay playsinline muted></video>
      <canvas id="canvas"></canvas>
      <div class="scan-frame"><div class="scan-line"></div></div>
    </div>

    <div id="divEstado" class="estado espera">Presiona el botón para activar la cámara</div>
    <button id="btnActivar" type="button" onclick="activarCamara()">Activar cámara</button>
  </div>

  <!-- Form oculto que hace postback cuando se lee el OTP -->
  <form id="frmOtp" runat="server">
    <asp:HiddenField ID="hdnOtp" runat="server" ClientIDMode="Static"/>
    <asp:Button ID="btnValidar" runat="server" Text="Validar"
                ClientIDMode="Static" OnClick="btnValidar_Click"/>
  </form>

  <!-- jsQR: librería ligera sin dependencias para decodificar QR -->
  <script src="https://cdn.jsdelivr.net/npm/jsqr@1.4.0/dist/jsQR.min.js"></script>
  <script>
    let video   = document.getElementById('video');
    let canvas  = document.getElementById('canvas');
    let ctx     = canvas.getContext('2d');
    let estado  = document.getElementById('divEstado');
    let escaneando = false;
    let yaDetectado = false;

    function activarCamara() {
      document.getElementById('btnActivar').disabled = true;
      navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' } })
        .then(stream => {
          video.srcObject = stream;
          video.play();
          escaneando = true;
          estado.className = 'estado espera';
          estado.textContent = 'Cámara activa — apunta el QR al visor…';
          requestAnimationFrame(tick);
        })
        .catch(err => {
          estado.className = 'estado error';
          estado.textContent = 'No se pudo acceder a la cámara: ' + err.message;
          document.getElementById('btnActivar').disabled = false;
        });
    }

    function tick() {
      if (!escaneando || yaDetectado) return;

      if (video.readyState === video.HAVE_ENOUGH_DATA) {
        canvas.height = video.videoHeight;
        canvas.width  = video.videoWidth;
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        let imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        let code = jsQR(imageData.data, imageData.width, imageData.height, {
          inversionAttempts: 'dontInvert'
        });

        if (code && code.data.startsWith('OTP:')) {
          let otp = code.data.replace('OTP:', '').trim();
          yaDetectado = true;
          escaneando  = false;
          // Detener stream
          video.srcObject.getTracks().forEach(t => t.stop());

          estado.className = 'estado ok';
          estado.textContent = '✅ QR detectado, verificando…';

          // Enviar al servidor
          document.getElementById('hdnOtp').value = otp;
          document.getElementById('btnValidar').click();
          return;
        }
      }
      requestAnimationFrame(tick);
    }
  </script>
</body>
</html>
