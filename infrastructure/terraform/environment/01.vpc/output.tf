output "vpc_id" {
  value = aws_vpc.booking.id
}

output "subnet_ids" {
  value = [aws_subnet.booking-a.id, aws_subnet.booking-b.id]
}