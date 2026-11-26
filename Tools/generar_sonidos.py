"""Biblioteca de audio original sintetizada para el juego. Solo utiliza la biblioteca estándar."""
from pathlib import Path
import array
import math
import random
import wave

OUT = Path(__file__).resolve().parents[1] / 'Assets/Resources/AudioCampus'
RATE = 24000
random.seed(707)

def save(name, duration, fn, loop=False):
    values = []
    for i in range(round(duration*RATE)):
        t=i/RATE
        fade=1 if loop else min(1,t/.008,(duration-t)/.035)
        values.append(max(-.85,min(.85,fn(t)*max(0,fade))))
    pcm=array.array('h',(round(v*32767) for v in values))
    OUT.mkdir(parents=True,exist_ok=True)
    with wave.open(str(OUT/(name+'.wav')),'wb') as f:
        f.setnchannels(1); f.setsampwidth(2); f.setframerate(RATE); f.writeframes(pcm.tobytes())

def tone(f,t): return math.sin(2*math.pi*f*t)
def notes(name, frequencies, step=.12):
    def sound(t):
        n=min(len(frequencies)-1,int(t/step)); u=t%step
        return .40*(tone(frequencies[n],u)+.2*tone(frequencies[n]*2,u))*math.exp(-u*14)
    save(name,len(frequencies)*step+.1,sound)

notes('ui_click',[880],.055)
notes('ui_abrir',[440,660],.07)
notes('ui_cerrar',[660,440],.06)
notes('dialogo',[520,620],.075)
notes('recoger',[660,880,1100],.08)
notes('mision',[523.25,659.25,783.99,1046.5],.13)
notes('correcto',[660,990],.10)
notes('error',[180,135],.12)
notes('huella',[800,1000,1200],.10)
notes('ascensor_llegada',[660,523.25],.24)
notes('curar',[440,554.36,659.25],.15)
notes('tarjeta',[1100,1400],.09)
notes('transmision',[740,960,740,1100,1480],.1)
notes('energia',[90,180,360,720],.16)
notes('victoria',[392,523.25,659.25,783.99,1046.5],.22)
notes('muerte',[240,202,160,100,60],.21)
for n in (1,2):
    save('paso_'+str(n),.16,lambda t:.32*(random.uniform(-1,1)*math.exp(-t*38)+.3*tone(130+20*n,t)*math.exp(-t*28)))
    save('metal_'+str(n),.23,lambda t:.24*(random.uniform(-1,1)*math.exp(-t*55)+tone(330+45*n,t)*math.exp(-t*24)))
save('puerta',.65,lambda t:.24*(random.uniform(-1,1)*.7+tone(94+30*t,t))*math.sin(math.pi*t/.65)**2)
save('ascensor_motor',1.6,lambda t:.22*(tone(55,t)+.2*tone(113,t)+.12*random.uniform(-1,1))*math.sin(math.pi*t/1.6))
save('golpe',.36,lambda t:.44*(random.uniform(-1,1)*.65+tone(78-80*t,t))*math.exp(-t*10))
save('escudo',.45,lambda t:.34*(tone(950-650*t,t)+.25*random.uniform(-1,1))*math.exp(-t*8))
save('pulso',.7,lambda t:.35*(tone(1500-1500*t,t)+.4*tone(240,t))*math.exp(-t*5))
save('aturdido',.4,lambda t:.22*tone(280+700*t,t)*math.exp(-t*7))
save('electrico',.55,lambda t:.30*(random.uniform(-1,1)*.7+tone(100,t))*math.exp(-t*5))
save('toxico',.65,lambda t:.22*(random.uniform(-1,1)*.5+tone(180+30*math.sin(32*t),t))*math.sin(math.pi*t/.65))
save('alarma',.9,lambda t:.24*tone(440+200*math.sin(10*t),t)*math.sin(math.pi*t/.9))
save('monstruo',1.0,lambda t:.28*(tone(65+12*math.sin(14*t),t)+.3*tone(131,t)+.2*random.uniform(-1,1))*math.sin(math.pi*t))
save('alerta_monstruo',.65,lambda t:.30*(tone(95+65*t,t)+.3*tone(225,t))*math.sin(math.pi*t/.65))
save('latido',.6,lambda t:.28*tone(65,t)*(math.exp(-((t-.08)/.055)**2)+.7*math.exp(-((t-.25)/.06)**2)))
save('portal',1.7,lambda t:.22*(tone(120+80*t,t)+.6*tone(240+120*t,t)+.2*random.uniform(-1,1))*math.sin(math.pi*t/1.7))
save('desmayo',1.8,lambda t:.22*(tone(190-80*t,t)+.2*random.uniform(-1,1))*math.sin(math.pi*t/1.8))
save('anclajes',.8,lambda t:.34*(tone(160,t)+.5*random.uniform(-1,1))*sum(math.exp(-max(0,t-p)*45) if t>=p else 0 for p in (.02,.25,.5)))
# Los senos usan ciclos enteros en 16 segundos para bucles sin saltos.
for name,base in [('amb_menu',110),('amb_campus',196),('amb_interior',80),('amb_vacio',55),('amb_pisox',40)]:
    def ambience(t,b=base,n=name):
        a=.5+.2*math.sin(2*math.pi*t/16)
        frequencies=(b,b*1.5,b*2) if n in ('amb_campus','amb_menu') else (b,b+1,b*2+.25)
        return .09*a*sum(tone(f,t)*v for f,v in zip(frequencies,(1,.4,.2)))
    save(name,16,ambience,True)
print('Generados',len(list(OUT.glob('*.wav'))),'sonidos WAV originales a',RATE,'Hz.')
