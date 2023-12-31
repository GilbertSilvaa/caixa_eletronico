﻿using api.DAO;
using api.Decorators;
using api.Models;
using api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransacaoController : ControllerBase
    {
        private readonly IClienteRepository _clienteDAO;

        public TransacaoController()
        {
            _clienteDAO = new ClienteDAO();
        }

        [Route("deposito")]
        [HttpPost]
        public ActionResult<Transacao> Depositar([FromBody] TransacaoDados transacao)
        {
            var _transacaoDAO = new TransacaoDAO(TipoTransacao.deposito);
            var transacaoResponse = _transacaoDAO.ExecutarTransacao(transacao);

            if (transacaoResponse == null) return BadRequest(); 

            return Ok(transacaoResponse);
        }

        [Route("saque")]
        [HttpPost]
        public ActionResult<Transacao> Sacar([FromBody] TransacaoDados transacao)
        {
            var _transacaoDAO = new TransacaoDAO(TipoTransacao.saque);
            var _transacaoDecorator = new ValidateTransacaoDecorator(_transacaoDAO, _clienteDAO);
            var transacaoResponse = _transacaoDecorator.ExecutarTransacao(transacao);

            if (transacaoResponse == null) return BadRequest();

            return Ok(transacaoResponse);
        }

        [Route("transferencia")]
        [HttpPost]
        public ActionResult<Transacao> Transferir([FromBody] TransacaoDados transacao)
        {
            var _transacaoDAO = new TransacaoDAO(TipoTransacao.tranferencia);
            var _transacaoDecorator = new ValidateTransacaoDecorator(_transacaoDAO, _clienteDAO);
            var transacaoResponse = _transacaoDecorator.ExecutarTransacao(transacao);

            if (transacaoResponse == null) return BadRequest();

            return Ok(transacaoResponse);
        }

        [Route("buscar")]
        [HttpGet]
        public ActionResult<List<TransacaoRegistro>> BuscarTransacoes([FromQuery] int idCliente)
        {
            var _transacaoTransf = new TransacaoDAO(TipoTransacao.tranferencia);
            var _transacaoSaque = new TransacaoDAO(TipoTransacao.saque);
            var _transacaoDeposito = new TransacaoDAO(TipoTransacao.deposito);

            List<List<TransacaoRegistro>?> responses = new()
            { 
                _transacaoTransf.BuscarTransacoesCliente(idCliente),
                _transacaoSaque.BuscarTransacoesCliente(idCliente),
                _transacaoDeposito.BuscarTransacoesCliente(idCliente)
            };

            var registros = ValidateTransacaoDecorator.OrderTransacoesPorData(responses!);

            if (registros == null) return BadRequest();

            return Ok(registros);
        }
    }
}
