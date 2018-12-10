function [positionState,a_hl_n,a_hr_n,a_kl_n,a_kr_n]=gait_normal(gait_length,gait_height)
t=0:0.005:2; %采样时间
l1=0.4; l2=0.4;%l1是大腿长度，l2是小腿长度
z=gait_height; %z为步高
s=gait_length; %s为步长
T=2;   %T为步态周期长度
v=s/(T/2);
sb=s/2;
positionState = 'normal';
for i=1:201
xa_n(i)=(-2*s)*t(i)^3+(3*s)*t(i)^2; %xa,ya为右脚位置
ya_n(i)=(z/2)*sin((2*pi/(s))*(xa_n(i))-(pi/2))+(z/2);
x0_n(i)=0.5*((-2*s)*t(i)^3+3*s*t(i)^2)+sb/2;%x0,y0为髋关节位置
y0_n(i)=(((l1+l2)-sqrt((l1+l2)^2-(s/4)^2))/2)*sin((2*pi/(s/2))*(x0_n(i)-sb/2)-(pi/2))+(l1+l2+sqrt((l1+l2)^2-(s/4)^2))/2;
xc_n(i)=sb; %xc,yc为左脚位置
yc_n(i)=0;
end 
for i=201:401
xa_n(i)=s; %xa,ya为右脚位置
ya_n(i)=0;
x0_n(i)=0.5*((-2*s)*(t(i)-1)^3+3*s*(t(i)-1)^2)+sb+s/4;%x0,y0为髋关节位置
y0_n(i)=(((l1+l2)-sqrt((l1+l2)^2-(s/4)^2))/2)*sin((2*pi/(s/2))*(x0_n(i)-sb-s/4)-(pi/2))+(l1+l2+sqrt((l1+l2)^2-(s/4)^2))/2;
xc_n(i)=sb+(-2*s)*(t(i)-1)^3+(3*s)*(t(i)-1)^2; %xc,yc为左脚位置
yc_n(i)=(z/2)*sin((2*pi/(s))*(xc_n(i)-sb)-(pi/2))+(z/2);
end 

for i=1:401 %求解右膝关节位置
    l=sqrt((xa_n(i)-x0_n(i))^2+(ya_n(i)-y0_n(i))^2);
    theat1=acos((l2^2+l^2-l1^2)/(2*l*l2));
    theat2=atan((y0_n(i)-ya_n(i))/(x0_n(i)-xa_n(i)));
    if(xa_n(i)<x0_n(i))
    x1=(xa_n(i)+l2*cos(theat2-theat1));
    x2=(xa_n(i)+l2*cos(theat2+theat1));
    y1=(ya_n(i)+abs(l2*sin(theat2-theat1)));
    y2=(ya_n(i)+abs(l2*sin(theat2+theat1)));
    else
       x1=(xa_n(i)-l2*cos(theat2-theat1));
    x2=(xa_n(i)-l2*cos(theat2+theat1));
    y1=(ya_n(i)+abs(l2*sin(theat2-theat1)));
    y2=(ya_n(i)+abs(l2*sin(theat2+theat1))); 

    end  
        xb_n(i)=max(x1,x2);
    yb_n(i)=max(y1,y2);
end
for i=1:401 %求解左膝关节位置
 l=sqrt((xc_n(i)-x0_n(i))^2+(yc_n(i)-y0_n(i))^2);
    theat1=acos((l2^2+l^2-l1^2)/(2*l*l2));
    theat2=atan((y0_n(i)-yc_n(i))/(x0_n(i)-xc_n(i)));
    if(xc_n(i)>x0_n(i))
    x3=real(xc_n(i)-l2*cos(theat2-theat1));
    x4=real(xc_n(i)-l2*cos(theat2+theat1));
    y3=real(yc_n(i)+abs(l2*sin(theat2-theat1)));
    y4=real(yc_n(i)+abs(l2*sin(theat2+theat1)));
    else
     x3=real(xc_n(i)+l2*cos(theat2-theat1));
    x4=real(xc_n(i)+l2*cos(theat2+theat1));
    y3=real(yc_n(i)+abs(l2*sin(theat2-theat1)));
    y4=real(yc_n(i)+abs(l2*sin(theat2+theat1)));   
  
    end
      xd_n(i)=max(x3,x4);
    yd_n(i)=max(y3,y4);
end

for i=1:401
a_hl_n(i)=atan((xd_n(i)-x0_n(i))/(y0_n(i)-yd_n(i)));
a_hl_n(i)=a_hl_n(i)*180/pi;
end
for i=1:401
a_hr_n(i)=atan((xb_n(i)-x0_n(i))/(y0_n(i)-yb_n(i)));
a_hr_n(i)=a_hr_n(i)*180/pi;
end
for i=1:401
a_kr_n(i)=atan((xa_n(i)-xb_n(i))/(yb_n(i)-ya_n(i)));
a_kr_n(i)=a_hr_n(i)-(a_kr_n(i)*180/pi);
if(a_kr_n(i)<0)
    a_kr_n(i)=0;
end
end
for i=1:401
a_kl_n(i)=atan((xc_n(i)-xd_n(i))/(yd_n(i)-yc_n(i)));
a_kl_n(i)=a_hl_n(i)-(a_kl_n(i)*180/pi);
if(a_kl_n(i)<0)
    a_kl_n(i)=0;
end
end  
a_hr_n=real(a_hr_n);
a_kr_n=real(a_kr_n);
end