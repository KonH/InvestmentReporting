<template>
	<b>{{ currency.code }}</b>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { CurrencyDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'Currency',
})
export default class Currency extends Vue {
	@Prop()
	currency!: CurrencyDto;
}
</script>
