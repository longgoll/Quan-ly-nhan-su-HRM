// Định nghĩa kiểu cho toast
interface ToastProps {
  title: string;
  description?: string;
  variant?: 'default' | 'destructive';
}

// Hook đơn giản để tương thích với hệ thống hiện tại
export function useToast() {
  return {
    toast: ({ title, description, variant = 'default' }: ToastProps) => {
      // Sử dụng native browser alert cho đơn giản
      // Sau này có thể thay thế bằng toast library khác
      if (variant === 'destructive') {
        alert(`Error: ${title}${description ? `\n${description}` : ''}`);
      } else {
        alert(`Success: ${title}${description ? `\n${description}` : ''}`);
      }
    }
  };
}
