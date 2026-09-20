import type { CSSProperties, ReactNode } from "react";

type ImageFrameProps = {
  src: string;
  alt: string;
  className?: string;
  imageClassName?: string;
  children?: ReactNode;
};

export function ImageFrame({ src, alt, className = "", imageClassName = "", children }: ImageFrameProps) {
  const backgroundStyle: CSSProperties = { backgroundImage: `url(${JSON.stringify(src)})` };

  return (
    <span className={`image-frame ${className}`.trim()}>
      <span className="image-frame-background" style={backgroundStyle} aria-hidden="true" />
      <span className="image-frame-overlay" aria-hidden="true" />
      <img className={`image-frame-foreground ${imageClassName}`.trim()} src={src} alt={alt} />
      {children}
    </span>
  );
}
