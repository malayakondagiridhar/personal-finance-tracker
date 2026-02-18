export function Skeleton({ className = '' }: { className?: string }) {
  return <div className={`animate-pulse rounded bg-gray-200 ${className}`.trim()} />
}

export function CardSkeleton() {
  return (
    <div className="rounded-lg bg-white p-4 shadow-sm">
      <Skeleton className="h-3 w-24" />
      <Skeleton className="mt-2 h-7 w-32" />
    </div>
  )
}
