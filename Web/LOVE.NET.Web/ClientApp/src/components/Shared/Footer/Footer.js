export default function Footer() {
  const year = new Date().getFullYear();

  return (
    <div className='p-3'>
        © {year} Copyright <strong>Iwan Mitowski</strong>
    </div>
  );
}
